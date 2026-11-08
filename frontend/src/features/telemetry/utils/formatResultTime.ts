import { formatDuration } from './formatDuration'

export function formatLapTime(seconds: number | null): string {
  return formatDuration(seconds)
}

export function formatTotalTime(seconds: number | null): string {
  if (seconds === null || !Number.isFinite(seconds) || seconds < 0) return '—'

  const milliseconds = Math.round(seconds * 1000)
  const hours = Math.floor(milliseconds / 3600000)
  const minutes = Math.floor(milliseconds % 3600000 / 60000)
  const remainingSeconds = Math.floor(milliseconds % 60000 / 1000)
  const fraction = milliseconds % 1000

  if (hours === 0) return formatDuration(seconds)

  return `${hours}:${String(minutes).padStart(2, '0')}:${String(remainingSeconds).padStart(2, '0')}.${String(fraction).padStart(3, '0')}`
}

export function formatGap(seconds: number | null): string {
  if (seconds === null || !Number.isFinite(seconds) || seconds < 0) return '—'
  return `+${seconds.toFixed(3)}`
}
