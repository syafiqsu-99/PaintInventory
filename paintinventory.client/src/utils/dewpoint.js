const B = 17.62
const C = 243.12

export function dewPoint(airTempC, humidityPct) {
  if (airTempC === '' || airTempC == null || humidityPct === '' || humidityPct == null) return null
  const t = Number(airTempC)
  const rh = Number(humidityPct)
  if (!Number.isFinite(t) || !Number.isFinite(rh) || rh <= 0 || rh > 100) return null
  const gamma = Math.log(rh / 100) + (B * t) / (C + t)
  return Math.round(((C * gamma) / (B - gamma)) * 10) / 10
}

export function belowDewMargin(substrateTempC, dewPointC, margin = 3) {
  if (substrateTempC === '' || substrateTempC == null || dewPointC === '' || dewPointC == null) return false
  return Number(substrateTempC) - Number(dewPointC) < margin
}
