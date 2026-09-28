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
import LoginView from '@/views/LoginView.vue'
import { useAuthStore } from '@/store/auth'

const routes = [
  { path: '/login', name: 'login', component: LoginView, meta: { public: true } },
  { path: '/', name: 'scan', component: ScanView },
  { path: '/transfer', name: 'transfer', component: TransferView, meta: { staff: true } },
  { path: '/stock-level', name: 'stock-level', component: StockLevelView },
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
  const auth = useAuthStore()
  try {
    await auth.load()
  } catch {
    return true
  }

  if (to.meta.public) return auth.isAuthenticated && to.name === 'login' ? { name: 'scan' } : true
  if (!auth.isAuthenticated) return { name: 'login', query: to.fullPath !== '/' ? { redirect: to.fullPath } : {} }
  if (to.meta.staff && !auth.isStaff) return { name: 'scan' }
  return true
})

export default router
