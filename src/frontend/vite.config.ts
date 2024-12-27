import { sveltekit } from "@sveltejs/kit/vite";

import { version } from "./constants.json";
import { defineConfig, type UserConfig } from "vite";

export default defineConfig(({ command }) => {
  const result: UserConfig = {
    plugins: [sveltekit()],

    define: {
      __DEEVENUE_VERSION__: JSON.stringify(version),
    },
    envPrefix: "DEEVENUE_",
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
