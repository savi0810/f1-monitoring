import type { Session } from '../types'

export function sessionLabel(session: Session): string {
  const name = session.sessionName.trim().toLowerCase()
  const type = session.sessionType.trim().toLowerCase()

  const description = `${name} ${type}`
  if (description.includes('sprint') && (description.includes('qualifying') || description.includes('shootout'))) {
    return 'Квалификация спринта'
  }
  if (name.includes('sprint') || type === 'sprint') return 'Спринт'
  if (name.includes('qualifying') || type === 'qualifying') return 'Квалификация'
  if (name === 'race' || type === 'race') return 'Гонка'

  const practice = name.match(/practice\s*([123])/)
  if (practice) return `Практика ${practice[1]}`
  if (type === 'practice') return 'Практика'

  return session.sessionName
}
