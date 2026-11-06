import type { Session, SessionMeeting, SessionYear } from '../types'

export function buildSessionTree(sessions: Session[], now = Date.now()): SessionYear[] {
  const years = new Map<number, Map<number, Session[]>>()

  for (const session of sessions) {
    const start = Date.parse(session.dateStart)
    const end = Date.parse(session.dateEnd)
    if (!Number.isFinite(start) || !Number.isFinite(end) || end > now) continue

    let meetings = years.get(session.year)
    if (!meetings) {
      meetings = new Map()
      years.set(session.year, meetings)
    }

    const meetingSessions = meetings.get(session.meetingKey) ?? []
    meetingSessions.push(session)
    meetings.set(session.meetingKey, meetingSessions)
  }

  return [...years.entries()]
    .sort(([left], [right]) => right - left)
    .map(([year, meetings]) => ({
      year,
      meetings: [...meetings.entries()]
        .map(([meetingKey, meetingSessions]): SessionMeeting => {
          const sorted = meetingSessions.sort(
            (left, right) => Date.parse(left.dateStart) - Date.parse(right.dateStart),
          )

          return {
            meetingKey,
            name: sorted[0]?.circuitShortName || sorted[0]?.countryName || `Уикенд #${meetingKey}`,
            sessions: sorted,
          }
        })
        .sort(
          (left, right) =>
            Date.parse(right.sessions[0]!.dateStart) - Date.parse(left.sessions[0]!.dateStart),
        ),
    }))
}
