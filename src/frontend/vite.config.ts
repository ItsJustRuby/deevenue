import { defineConfig } from "vitest/config";
import { sveltekit } from "@sveltejs/kit/vite";

import { version } from "./constants.json";
import { type UserConfig } from "vite";

export default defineConfig(({ command }) => {
  const result: UserConfig = {
    plugins: [sveltekit()],

    define: {
      __DEEVENUE_VERSION__: JSON.stringify(version),
    },
    envPrefix: "DEEVENUE_",

    test: {
      include: ["src/**/*.{test,spec}.{js,ts}"],
    },
  };

  if (command === "serve") {
    result.server = {
      watch: {
        usePolling: true,
      },
      port: 3000,
    };
  }

  return result;
});
