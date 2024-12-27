import type { NotificationViewModel } from "$lib/api/models";

interface Notification extends NotificationViewModel {
  timestamp: Date;
}

export const notifications = $state<Notification[]>([]);

export const addNotification = (notification: NotificationViewModel) => {
  const hasAnyVisibleParts = notification.contents.some(
    (p) => p.type === "link" || p.type === "text",
  );

  if (hasAnyVisibleParts)
    notifications.push({
      ...notification,
      timestamp: new Date(),
    });
};

export const dismissNotification = (index: number) => {
  notifications.splice(index, 1);
};
