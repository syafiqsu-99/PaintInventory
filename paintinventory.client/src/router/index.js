import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '@/views/HomeView.vue'
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
import { useAuthStore } from '@/store/auth'

const routes = [
  { path: '/', name: 'home', component: HomeView, meta: { fullBleed: true } },
  { path: '/scan', name: 'scan', component: ScanView },
  { path: '/stock-level', name: 'stock-level', component: StockLevelView, meta: { staff: true } },
  { path: '/transfer', name: 'transfer', component: TransferView, meta: { staff: true } },
  { path: '/dashboard', name: 'dashboard', component: DashboardView, meta: { staff: true } },
  { path: '/products', name: 'products', component: ProductsView, meta: { staff: true } },
  { path: '/vendors', name: 'vendors', component: VendorsView, meta: { staff: true } },
  { path: '/settings', name: 'settings', component: SettingsView, meta: { staff: true } },
  { path: '/reports', name: 'reports', component: ReportListView, meta: { staff: true } },
  { path: '/reports/new', name: 'report-new', component: ReportEditView, meta: { staff: true } },
  { path: '/reports/:id', name: 'report-edit', component: ReportEditView, meta: { staff: true } },
  { path: '/:pathMatch(.*)*', name: 'not-found', component: NotFoundView }
]

const router = createRouter({ history: createWebHistory(), routes })

router.beforeEach(async (to) => {
  if (!to.meta.staff) return true

  const auth = useAuthStore()
  if (!auth.loaded) {
    try { await auth.load() } catch { /* boot overlay reports connectivity */ }
  }
  if (auth.isStaff) return true

  auth.requestUnlock(to.fullPath)
  return { name: 'home' }
})

export default router
