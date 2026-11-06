export function formatDuration(seconds: number | null): string {
  if (seconds === null || !Number.isFinite(seconds) || seconds < 0) return '—'

  const milliseconds = Math.round(seconds * 1000)
  const minutes = Math.floor(milliseconds / 60000)
  const remainingSeconds = Math.floor(milliseconds % 60000 / 1000)
  const fraction = milliseconds % 1000

  const secondsText = String(remainingSeconds).padStart(minutes > 0 ? 2 : 1, '0')
  const fractionText = String(fraction).padStart(3, '0')

  return `${minutes > 0 ? `${minutes}:` : ''}${secondsText}.${fractionText}`
}
