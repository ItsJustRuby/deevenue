<script lang="ts">
  import type { MediumViewModel } from "$lib/api/models";

  const { mediaKind, id }: Pick<MediumViewModel, "mediaKind" | "id"> = $props();

  let isSmall = $state(true);
  let fullsizeableElement = $state<HTMLElement | null>(null);

  const url = $derived(`/api/file/${id}`);

  const globalFullscreenHotkey = async (e: KeyboardEvent) => {
    if (e.key !== "f") return;

    const isAnythingFocused =
      document.hasFocus() &&
      document.activeElement !== null &&
      document.activeElement !== document.body &&
      document.activeElement !== document.documentElement;

    if (!isAnythingFocused || document.activeElement === fullsizeableElement) {
      if (!document.fullscreenElement) {
        await fullsizeableElement?.requestFullscreen();
      } else {
        await document.exitFullscreen();
      }
    }
  };
</script>

<svelte:document onkeypress={globalFullscreenHotkey} />

{#if mediaKind !== "unusable"}
  <button
    onclick={() => {
      isSmall = !isSmall;
    }}
  >
    {#if mediaKind === "image"}
      <img
        src={url}
        class:max-h-[90vh]={isSmall}
        class:max-w-none={!isSmall}
        alt="Medium"
        bind:this={fullsizeableElement}
      />
    {:else}
      <video controls src={url} bind:this={fullsizeableElement}>
        <track kind="captions" />
      </video>
    {/if}
  </button>
{/if}
