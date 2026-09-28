import { defineStore } from 'pinia'
import http from '@/utils/http'

export const useStockStore = defineStore('stock', () => {
  function stockIn(payload) {
    return http.post('/stock/in', payload)
  }

  function stockOut(payload) {
    return http.post('/stock/out', payload)
  }

  function adjust(payload) {
    return http.post('/stock/adjust', payload)
  }

  function transfer(payload) {
    return http.post('/stock/transfer', payload)
  }

  function receive(payload) {
    return http.post('/stock/receive', payload)
  }

  function inTransit(vendorId = null) {
    return http.get(vendorId ? `/stock/in-transit?vendorId=${vendorId}` : '/stock/in-transit')
  }

  return { stockIn, stockOut, adjust, transfer, receive, inTransit }
})
