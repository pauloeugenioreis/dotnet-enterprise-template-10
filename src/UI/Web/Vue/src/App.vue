<template>
  <Loading />
  <router-view></router-view>
</template>

<script setup lang="ts">
import Loading from './components/Loading.vue';
</script>

<style>
@import url("https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800;900&display=swap");

@tailwind base;
@tailwind components;
@tailwind utilities;

body {
  margin: 0;
  font-family: "Inter", sans-serif;
  background-color: #f8fafc;
}

.animate-fade-in {
  animation: fadeIn 0.6s ease-out;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
