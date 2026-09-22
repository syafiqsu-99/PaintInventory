import { defineStore } from 'pinia'
import http from '@/utils/http'

// Entity is 'products' or 'vendors' — matching the server route segments.
export const useImportExportStore = defineStore('importExport', () => {
  function preview(entity, file) {
    const fd = new FormData()
    fd.append('file', file)
    return http.upload(`/importexport/${entity}/preview`, fd)
  }

  function commit(entity, file, mode) {
    const fd = new FormData()
    fd.append('file', file)
    fd.append('mode', mode)
    return http.upload(`/importexport/${entity}/commit`, fd)
  }

  const exportUrl = (entity) => `/api/importexport/${entity}/export`
  const templateUrl = (entity) => `/api/importexport/${entity}/template`

  return { preview, commit, exportUrl, templateUrl }
})
