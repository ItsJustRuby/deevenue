import { goto } from "$app/navigation";
import { api } from "./api/client";
import { getEntityFromNotification } from "./notifications";

export const getRandomRuleViolation = async (): Promise<string | null> => {
  const res = await api.GET("/rule/violation/random");

  // Should never happen
  if (res.error) return Promise.reject();

  const entity = getEntityFromNotification(res.data);
  if (entity) {
    await goto(`/show/${entity.id}`);
    return null;
  }

  return "/";
};
