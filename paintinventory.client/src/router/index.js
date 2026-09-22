import { createRouter, createWebHistory } from 'vue-router'
import StockInView from '@/views/StockInView.vue'
import StockOutView from '@/views/StockOutView.vue'
import TransferView from '@/views/TransferView.vue'
import StockLevelView from '@/views/StockLevelView.vue'
import ProductsView from '@/views/ProductsView.vue'
import VendorsView from '@/views/VendorsView.vue'
import DashboardView from '@/views/DashboardView.vue'
import ReportListView from '@/views/ReportListView.vue'
import ReportEditView from '@/views/ReportEditView.vue'
import NotFoundView from '@/views/NotFoundView.vue'

const routes = [
  { path: '/', name: 'stock-in', component: StockInView },
  { path: '/stock-out', name: 'stock-out', component: StockOutView },
  { path: '/transfer', name: 'transfer', component: TransferView },
  { path: '/stock-level', name: 'stock-level', component: StockLevelView },
  { path: '/reports', name: 'reports', component: ReportListView },
  { path: '/reports/new', name: 'report-new', component: ReportEditView },
  { path: '/reports/:id', name: 'report-edit', component: ReportEditView },
  { path: '/products', name: 'products', component: ProductsView },
  { path: '/vendors', name: 'vendors', component: VendorsView },
  { path: '/dashboard', name: 'dashboard', component: DashboardView },
  { path: '/:pathMatch(.*)*', name: 'not-found', component: NotFoundView }
]

export default createRouter({ history: createWebHistory(), routes })
