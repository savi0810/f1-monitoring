export interface Session {
  sessionKey: number
  meetingKey: number
  year: number
  sessionType: string
  sessionName: string
  circuitShortName: string | null
  countryName: string | null
  dateStart: string
  dateEnd: string
  isCancelled: boolean
  lapCount: number
}

export interface SessionMeeting {
  meetingKey: number
  name: string
  sessions: Session[]
}

export interface SessionYear {
  year: number
  meetings: SessionMeeting[]
}

export interface Lap {
  eventId: string
  sessionKey: number
  driverNumber: number | null
  lapNumber: number
  lapDuration: number | null
  sector1Duration: number | null
  sector2Duration: number | null
  sector3Duration: number | null
  isPitOutLap: boolean
  occurredAt: string | null
}

export const ImportStatus = {
  Pending: 0,
  Processing: 1,
  Completed: 2,
  Empty: 3,
  Failed: 4,
} as const

export type ImportStatus =
  typeof ImportStatus[keyof typeof ImportStatus]

export interface SessionImport {
  sessionKey: number
  importId: string
  status: ImportStatus
  error: string | null
}

export interface SessionFilters {
  year?: number
  type?: string
}
