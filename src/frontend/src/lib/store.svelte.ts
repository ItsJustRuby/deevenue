export { notifications } from "./notifications/store.svelte";

interface LastMediumUpdate {
  timestamp: Date | null;
}
export const lastMediumUpdate = $state<LastMediumUpdate>({
  timestamp: new Date(),
});

interface TSession {
  isSfw: boolean | null;
}
export const session = $state<TSession>({ isSfw: null });

interface SearchState {
  searchTerm: string;
}
export const searchState = $state<SearchState>({
  searchTerm: "",
});
