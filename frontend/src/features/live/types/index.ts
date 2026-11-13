export interface LiveDriverSnapshot {
  driverId: string
  fullName: string
  position: number | null
}

export interface LiveRaceSnapshot {
  eventId: string
  source: string
  sessionId: string
  observedAt: string
  scheduledAt: string
  sessionType: string
  sessionName: string
  state: string
  completed: boolean | null
  drivers: LiveDriverSnapshot[]
  expectedUpdateInterval: string | null
}

export interface CurrentLiveDto {
  snapshot: LiveRaceSnapshot | null
}

export interface SourceUpdated {
  sessionId: string
  observedAt: string
}

export interface PositionAlertTriggered {
  alertId: string
  userId: string
  subscriptionId: string
  eventId: string
  sessionId: string
  driverId: string
  driverName: string
  previousPosition: number
  currentPosition: number
  observedAt: string
}
