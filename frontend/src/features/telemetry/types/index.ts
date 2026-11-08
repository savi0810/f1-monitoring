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

export interface SessionCatalogMeeting {
  meetingKey: number
  meetingName: string | null
  sessions: Session[]
}

export interface SessionCatalogYear {
  year: number
  meetings: SessionCatalogMeeting[]
}

export interface SessionResult {
  driverNumber: number
  fullName: string | null
  teamName: string | null
  position: number | null
  numberOfLaps: number | null
  race: RaceResult | null
  practice: PracticeResult | null
  qualifying: QualifyingResult | null
  didNotFinish: boolean | null
  didNotStart: boolean | null
  disqualified: boolean | null
  laps: SessionResultLap[]
}

export interface RaceResult {
  totalTimeSeconds: number | null
  gapToLeaderSeconds: number | null
  gapToLeaderText: string | null
}

export interface PracticeResult {
  bestLapSeconds: number | null
  gapToLeaderSeconds: number | null
}

export interface QualifyingResult {
  phases: QualifyingPhaseResult[]
}

export interface QualifyingPhaseResult {
  phase: number
  bestLapSeconds: number | null
  gapToLeaderSeconds: number | null
}

export interface SessionResultLap {
  lapNumber: number
  lapDuration: number | null
  sector1Duration: number | null
  sector2Duration: number | null
  sector3Duration: number | null
  isPitOutLap: boolean
  dateStart: string | null
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
