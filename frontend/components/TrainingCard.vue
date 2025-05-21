<template>
  <div class="training-card">
    <h2>{{ menu.jpName }}</h2>
    <p>{{ menu.description }}</p>
    <div class="tags">
      <span
        v-for="tag in menu.tags"
        :key="tag.tagId"
        class="tag"
      >
        {{ tag.tagId }}
      </span>
    </div>
  </div>
</template>

<script setup>
defineProps({
  menu: {
    type: Object,
    required: true
  }
})
</script>

<style scoped>
.training-card {
  border: 1px solid #ccc;
  padding: 10px;
  margin-bottom: 12px;
  cursor: pointer;
}
.tag {
  margin-right: 6px;
  background: #eee;
  padding: 2px 6px;
  border-radius: 4px;
}
</style>
