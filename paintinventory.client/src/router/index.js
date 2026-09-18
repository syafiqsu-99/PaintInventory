import { createRouter, createWebHistory } from 'vue-router'
import ScanView from '@/views/ScanView.vue'
import InventoryView from '@/views/InventoryView.vue'
import DashboardView from '@/views/DashboardView.vue'
import NotFoundView from '@/views/NotFoundView.vue'

const routes = [
  { path: '/', name: 'scan', component: ScanView },
  { path: '/inventory', name: 'inventory', component: InventoryView },
  { path: '/dashboard', name: 'dashboard', component: DashboardView },
  { path: '/:pathMatch(.*)*', name: 'not-found', component: NotFoundView }
]

export default createRouter({ history: createWebHistory(), routes })
