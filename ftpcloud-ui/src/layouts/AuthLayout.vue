<template>
  <div class="auth-layout">
    <div class="auth-background"></div>
    <RouterView />
  </div>
</template>

<style scoped>
.auth-layout {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: var(--color-background);
  position: relative;
  overflow: hidden;
}

.auth-background {
  position: absolute;
  top: -50%;
  left: -50%;
  width: 200%;
  height: 200%;
  background: radial-gradient(circle at 50% 50%, var(--brand-primary-light) 0%, transparent 50%);
  opacity: 0.5;
  z-index: 0;
  pointer-events: none;
}

:deep(.login-card) {
  position: relative;
  z-index: 1;
}
</style>
