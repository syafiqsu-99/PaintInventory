// Parsers for keyboard-wedge scanner input, including OCR reads of printed labels.
// Label dates are day-first (DD.MM.YYYY / DD/MM/YYYY); month-first is never guessed.

const OCR_DIGIT_FIXES = { O: '0', Q: '0', D: '0', I: '1', L: '1', '|': '1', S: '5', B: '8', Z: '2' }
const DIGITISH = '[0-9OQDIL|SBZ]'

const DAY_FIRST = new RegExp(
  `(${DIGITISH}{1,2})\\s*[./-]\\s*(${DIGITISH}{1,2})\\s*[./-]\\s*(${DIGITISH}{4}|${DIGITISH}{2})(?![0-9])`)
const ISO = /(\d{4})-(\d{2})-(\d{2})/

const TEXT_PREFIX = /^(BATCH\s*NO|BATCH\s*NUMBER|BATCH|LOT\s*NO|LOT\s*NUMBER|LOT|B\/N|L\/N|BN)(?:\s*[.:#]\s*|\s+)/

function toDigits(s) {
  return s.replace(/[OQDIL|SBZ]/g, (c) => OCR_DIGIT_FIXES[c])
}

function pad(n) {
  return String(n).padStart(2, '0')
}

function toIso(year, month, day) {
  const d = new Date(Date.UTC(year, month - 1, day))
  if (d.getUTCFullYear() !== year || d.getUTCMonth() !== month - 1 || d.getUTCDate() !== day) return null
  return `${year}-${pad(month)}-${pad(day)}`
}

export function parseScannedDate(raw) {
  const text = String(raw ?? '').trim()
  if (!text) return { iso: null, raw: text, error: null }

  const upper = text.toUpperCase()

  const iso = upper.match(ISO)
  if (iso) {
    const value = toIso(Number(iso[1]), Number(iso[2]), Number(iso[3]))
    return value ? { iso: value, raw: text, error: null } : { iso: null, raw: text, error: 'Not a real date' }
  }

  const m = upper.match(DAY_FIRST)
  if (!m) return { iso: null, raw: text, error: 'Couldn’t read date — use DD.MM.YYYY or pick from calendar' }

  const day = Number(toDigits(m[1]))
  const month = Number(toDigits(m[2]))
  const yearDigits = toDigits(m[3])
  const year = yearDigits.length === 2 ? 2000 + Number(yearDigits) : Number(yearDigits)

  const value = toIso(year, month, day)
  return value
    ? { iso: value, raw: text, error: null }
    : { iso: null, raw: text, error: 'Not a real date — check day and month' }
}

export function formatDisplayDate(iso) {
  if (!iso) return ''
  const m = String(iso).match(/^(\d{4})-(\d{2})-(\d{2})/)
  return m ? `${m[3]}.${m[2]}.${m[1]}` : String(iso)
}

export function cleanScannedText(raw) {
  return String(raw ?? '')
    .trim()
    .toUpperCase()
    .replace(TEXT_PREFIX, '')
    .replace(/\s+/g, ' ')
    .trim()
}
