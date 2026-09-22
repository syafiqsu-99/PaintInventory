import { createRouter, createWebHistory } from 'vue-router'
import ScanView from '@/views/ScanView.vue'
import TransferView from '@/views/TransferView.vue'
import StockLevelView from '@/views/StockLevelView.vue'
import ProductsView from '@/views/ProductsView.vue'
import VendorsView from '@/views/VendorsView.vue'
import DashboardView from '@/views/DashboardView.vue'
import SettingsView from '@/views/SettingsView.vue'
import ReportListView from '@/views/ReportListView.vue'
import ReportEditView from '@/views/ReportEditView.vue'
import NotFoundView from '@/views/NotFoundView.vue'

const routes = [
  { path: '/', name: 'scan', component: ScanView },
  { path: '/transfer', name: 'transfer', component: TransferView },
  { path: '/stock-level', name: 'stock-level', component: StockLevelView },
  { path: '/dashboard', name: 'dashboard', component: DashboardView },
  { path: '/products', name: 'products', component: ProductsView },
  { path: '/vendors', name: 'vendors', component: VendorsView },
  { path: '/settings', name: 'settings', component: SettingsView },
  { path: '/reports', name: 'reports', component: ReportListView },
  { path: '/reports/new', name: 'report-new', component: ReportEditView },
  { path: '/reports/:id', name: 'report-edit', component: ReportEditView },
  { path: '/:pathMatch(.*)*', name: 'not-found', component: NotFoundView }
]

export default createRouter({ history: createWebHistory(), routes })
