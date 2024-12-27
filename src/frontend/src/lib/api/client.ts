import createClient from "openapi-fetch/src/index.js";
import type { paths } from "./oats/schema";
import { notificationMiddleware } from "$lib/notifications/middleware";

export const api = createClient<paths>({ baseUrl: "/api" });

api.use(notificationMiddleware);
