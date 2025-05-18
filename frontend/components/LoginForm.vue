<template>
  <form @submit.prevent="onSubmit">
    <div>
      <label for="email">Email</label>
      <input type="email" v-model="email" id="email" required />
    </div>
    <div>
      <label for="password">Password</label>
      <input type="password" v-model="password" id="password" required />
    </div>
    <button type="submit">Login</button>
  </form>
</template>

<script>
export default {
  data() {
    return {
      email: '',
      password: ''
    };
  },
  methods: {
    onSubmit() {
      this.$emit('submit', { email: this.email, password: this.password });
    }
  }
};
</script>
