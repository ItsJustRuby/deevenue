import { getContext, onMount, setContext } from "svelte";

const contextName = Symbol("titleParts");
type TitleParts = string[];

export const addTitlePart = (part: string) => {
  onMount(() => {
    const titleParts = getContext(contextName) as TitleParts;
    titleParts.unshift(part);
    return () => titleParts.shift();
  });
};

export const initializeTitleParts = (state: TitleParts) => {
  setContext(contextName, state);
};
