import type {
  INotificationPartViewModel,
  INotificationPartViewModelEntity,
  NotificationViewModel,
} from "$lib/api/models";

export const getEntityFromNotification = (
  notification: NotificationViewModel,
) => {
  const isMediumPart = (
    part: INotificationPartViewModel,
  ): part is INotificationPartViewModelEntity => {
    return part.type === "entity" && part.entityKind === "medium";
  };

  const mediumParts = notification.contents.filter(isMediumPart);
  if (mediumParts.length !== 1) return null;

  return mediumParts[0]!;
};
