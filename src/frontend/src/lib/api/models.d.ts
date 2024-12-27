import type { components, operations } from "./oats/schema";

type schemas = components["schemas"];

export type AllTagsViewModel = schemas["AllTagsViewModel"];

export type INotificationPartViewModel = schemas["INotificationPartViewModel"];
export type INotificationPartViewModelEntity =
  schemas["INotificationPartViewModelEntity"];

export type IRuleIff = schemas["IIffViewModel"];
export type IRuleThen = schemas["IThenViewModel"];

export type JobKindViewModel = schemas["JobKindViewModel"];
export type JobResultViewModel = schemas["JobResultViewModel"];
export type JobsViewModel = schemas["JobsViewModel"];

export type MediumRuleViolationsViewModel =
  schemas["MediumRuleViolationsViewModel"];
export type NotificationViewModel = schemas["NotificationViewModel"];
export type MediumViewModel = schemas["MediumViewModel"];
export type Rating = operations["setTagRating"]["parameters"]["path"]["rating"];
export type RulesViewModel = schemas["RulesViewModel"];
export type RuleViewModel = schemas["RuleViewModel"];
export type TagViewModel = schemas["TagViewModel"];
export type ThumbnailSheetViewModel = schemas["ThumbnailSheetViewModel"];

export type SearchResultViewModel = schemas["SearchResultViewModel"];
export type SearchResultsViewModel =
  schemas["PaginationViewModelOfSearchResultViewModel"];
