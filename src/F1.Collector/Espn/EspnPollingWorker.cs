using System.Diagnostics;
using System.Net;
using System.Text.Json;
using F1.Collector.Mapping;
using F1.Collector.Publishing;
using F1Monitor.Contracts;
using Microsoft.Extensions.Options;

namespace F1.Collector.Espn;

public class EspnPollingWorker : BackgroundService
{
    private readonly EspnClient _client;
    private readonly EspnSnapshotMapper _mapper;
    private readonly EspnPollingSchedule _schedule;
    private readonly LiveSnapshotPublisher _publisher;
    private readonly IOptionsMonitor<EspnOptions> _options;
    private readonly ILogger<EspnPollingWorker> _logger;

    public EspnPollingWorker(EspnClient client, EspnSnapshotMapper mapper, EspnPollingSchedule schedule, LiveSnapshotPublisher publisher, IOptionsMonitor<EspnOptions> options, ILogger<EspnPollingWorker> logger)
    {
        _client = client;
        _mapper = mapper;
        _schedule = schedule;
        _publisher = publisher;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        DateTimeOffset? scheduledAt = null;
        DateTimeOffset? finishedAt = null;
        LiveRaceSnapshot? pending = null;
        var finishedObservations = 0;
        var failures = 0;
        var exchangeDeclared = false;
        var selectedRace = (EventId: "", CompetitionId: "");

        while (!stoppingToken.IsCancellationRequested)
        {
            var options = _options.CurrentValue;
            var currentRace = (options.EventId, options.CompetitionId);

            if (currentRace != selectedRace)
            {
                selectedRace = currentRace;
                scheduledAt = null;
                finishedAt = null;
                pending = null;
                finishedObservations = 0;
                failures = 0;
            }

            var interval = _schedule.GetInterval(options, DateTimeOffset.UtcNow, scheduledAt, finishedAt);

            if (interval == null)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                continue;
            }

            var elapsed = Stopwatch.StartNew();

            try
            {
                if (!exchangeDeclared)
                {
                    await _publisher.DeclareExchangeAsync(stoppingToken);
                    exchangeDeclared = true;
                }

                if (pending == null)
                {
                    using var timeout = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                    timeout.CancelAfter(TimeSpan.FromSeconds(options.RequestTimeoutSeconds));

                    using var response = await _client.GetScoreboardAsync(timeout.Token);

                    if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        var retryAfter = response.Headers.RetryAfter?.Delta
                            ?? (response.Headers.RetryAfter?.Date - DateTimeOffset.UtcNow)
                            ?? TimeSpan.FromSeconds(5);

                        await Task.Delay(retryAfter > TimeSpan.Zero ? retryAfter : TimeSpan.FromSeconds(1), stoppingToken);
                        continue;
                    }

                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync(timeout.Token);
                    var observedAt = DateTimeOffset.UtcNow;
                    var scoreboard = JsonSerializer.Deserialize<EspnScoreboard>(json)
                        ?? throw new InvalidOperationException("ESPN scoreboard is empty.");

                    var raceEvent = scoreboard.Events?.FirstOrDefault(item => item.Id == options.EventId)
                        ?? throw new InvalidOperationException("ESPN event was not found.");

                    var competition = _mapper.FindCompetition(scoreboard, options.EventId, options.CompetitionId)
                        ?? throw new InvalidOperationException("ESPN competition was not found.");

                    pending = _mapper.MapSnapshot(competition, raceEvent.Name ?? "", Guid.NewGuid(), observedAt)
                        ?? throw new InvalidOperationException("ESPN competition is not a valid race.");

                    scheduledAt = pending.ScheduledAt;

                    if (pending.State == "post")
                    {
                        finishedObservations++;
                        if (finishedObservations >= 2)
                            finishedAt ??= observedAt;
                    }
                    else
                    {
                        finishedObservations = 0;
                        finishedAt = null;
                    }

                    pending = pending with
                    {
                        ExpectedUpdateInterval = _schedule.GetInterval(options, DateTimeOffset.UtcNow, scheduledAt, finishedAt)
                    };
                }

                await _publisher.PublishAsync(pending, stoppingToken);
                pending = null;
                failures = 0;

                interval = _schedule.GetInterval(_options.CurrentValue, DateTimeOffset.UtcNow, scheduledAt, finishedAt);
                var remaining = interval.GetValueOrDefault() - elapsed.Elapsed;
                if (remaining > TimeSpan.Zero)
                    await Task.Delay(remaining, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                failures++;
                _logger.LogWarning(ex, "ESPN polling or snapshot publication failed");
                var delay = TimeSpan.FromSeconds(Math.Min(failures, 10));
                await Task.Delay(delay, stoppingToken);
            }
        }
    }
}
