export default {
  "**/*": [
    "bun run check",
    "prettier --ignore-unknown --write",
    "eslint --fix",
  ],
};
