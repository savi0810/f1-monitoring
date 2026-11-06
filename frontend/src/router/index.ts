import { createRouter, createWebHistory } from 'vue-router'
import { hasValidToken } from '@/features/auth/tokenStorage'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', name: 'home', redirect: { name: 'sessions' } },
    { path: '/live', name: 'live', component: () => import('@/pages/LivePage.vue'), meta: { requiresAuth: true } },
    {
      path: '/sessions',
      component: () => import('@/pages/PastRacesPage.vue'),
      meta: { requiresAuth: true },
      children: [
        { path: '', name: 'sessions' },
        { path: ':sessionKey', name: 'session', component: () => import('@/features/telemetry/components/SessionDetails.vue') },
      ],
    },
    { path: '/login', name: 'login', component: () => import('@/pages/LoginPage/LoginPage.vue') },
    { path: '/register', name: 'register', component: () => import('@/pages/RegisterPage/RegisterPage.vue') },
  ],
})

router.beforeEach((to) => {
  const isAuthenticated = hasValidToken()

  if (to.meta.requiresAuth && !isAuthenticated) return { name: 'login' }
  if ((to.name === 'login' || to.name === 'register') && isAuthenticated) return { name: 'home' }
})

export default router
