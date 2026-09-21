const COAT_TYPES = ['Primer', 'SecondCoat', 'ThirdCoat', 'FourthCoat']
const COAT_LABELS = ['Primer', '2nd coat', '3rd coat', '4th coat']

export function coatTypeFor(sequence) {
  return COAT_TYPES[Math.min(sequence, 4) - 1]
}

export function coatLabel(sequence) {
  return COAT_LABELS[Math.min(sequence, 4) - 1]
}

function toDateInput(s) {
  return s ? String(s).substring(0, 10) : null
}

export function blankPrep() {
  return {
    gradeOfCleanliness: null, requiredRoughness: null, measuredRoughness: null,
    humidityPct: null, airTempC: null, substrateTempC: null, dewPointC: null,
    operator: null, date: null
  }
}

export function blankCoat(sequence = 1) {
  return {
    coatType: coatTypeFor(sequence), sequence,
    partAProductId: null, partBProductId: null, paintIdText: null,
    partABatch: null, partBBatch: null, shade: null,
    requiredThicknessUm: null, measuredThicknessUm: null,
    humidityPct: null, airTempC: null, substrateTempC: null, dewPointC: null,
    operator: null, date: null,
    deductFromStock: false, stockLocationVendorId: null,
    partAQtyUsed: null, partBQtyUsed: null
  }
}

export function blankItem(itemNo = 1) {
  return {
    itemNo, serialNumber: null, paintingSpec: null,
    componentDescription: null, componentLabel: null,
    abrasiveBlasting: true, requiredTotalDftUm: null, measuredTotalDftUm: null,
    adhesionTestPerformed: false, adhesionTestType: 'None',
    mekTestNotes: null, otherRemarks: null,
    blastVendorId: null, paintingVendorId: null,
    surfacePrep: blankPrep(), coats: [blankCoat(1)]
  }
}

export function blankReport() {
  return { ipo: '', customer: null, project: null, preparedBy: null, preparedDate: null, items: [blankItem(1)] }
}

export function fromDto(r) {
  return {
    ipo: r.ipo, customer: r.customer, project: r.project, preparedBy: r.preparedBy,
    preparedDate: toDateInput(r.preparedDate),
    items: r.items.map((i) => ({
      itemNo: i.itemNo, serialNumber: i.serialNumber, paintingSpec: i.paintingSpec,
      componentDescription: i.componentDescription, componentLabel: i.componentLabel,
      abrasiveBlasting: i.abrasiveBlasting,
      requiredTotalDftUm: i.requiredTotalDftUm, measuredTotalDftUm: i.measuredTotalDftUm,
      adhesionTestPerformed: i.adhesionTestPerformed ?? false,
      adhesionTestType: i.adhesionTestType ?? 'None',
      mekTestNotes: i.mekTestNotes, otherRemarks: i.otherRemarks,
      blastVendorId: i.blastVendorId, paintingVendorId: i.paintingVendorId,
      surfacePrep: i.surfacePrep ? { ...i.surfacePrep, date: toDateInput(i.surfacePrep.date) } : blankPrep(),
      coats: (i.coats ?? []).map((c) => ({ ...c, date: toDateInput(c.date) }))
    }))
  }
}
