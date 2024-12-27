import type { Middleware } from "openapi-fetch/src/index.js";
import { addNotification } from "./store.svelte";
import type { NotificationViewModel } from "$lib/api/models";

export const notificationMiddleware: Middleware = {
  onResponse: async ({ response }) => {
    if (response.headers.get("X-Deevenue-Schema") !== "Notification") return;

    // TIL this is necessary because reading the body of a response multiple times
    // raises a TypeError due to "disturbing the stream". Huh.
    const clone = response.clone();
    const notification = (await clone.json()) as NotificationViewModel;
    addNotification(notification);
  },
};
