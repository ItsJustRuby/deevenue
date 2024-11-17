<script lang="ts">
  import type { MediumViewModel } from "$lib/api/models";
  import { backendUrl, largeThumbnailMediaQuery } from "$lib/utils";
  import Spinner from "./spinner.svelte";
  import ThumbnailPicture from "./thumbnail-picture.svelte";

  const { id, mediaKind }: Pick<MediumViewModel, "id" | "mediaKind"> = $props();

  let videoRef = $state<HTMLVideoElement | undefined>(undefined);
  let pictureRef = $state<HTMLDivElement | undefined>(undefined);

  const startVideo = async () => {
    pictureRef!.classList.add("opacity-0");
    missingThumbnailRef?.classList.remove("opacity-0");
    try {
      await videoRef!.play();
      // eslint-disable-next-line @typescript-eslint/no-unused-vars
    } catch (e: unknown) {
      // Explicitly fine if the video src is 404.
    }
  };
  const rewindVideo = async () => {
    pictureRef!.classList.remove("opacity-0");
    missingThumbnailRef?.classList.add("opacity-0");
    try {
      videoRef!.pause();
      // eslint-disable-next-line @typescript-eslint/no-unused-vars
    } catch (e: unknown) {
      // Explicitly fine if the video src is 404.
    }
    videoRef!.currentTime = 0;
  };

  const getVideoSrc = (windowParam: Window, id: string) => {
    const size = windowParam.matchMedia(largeThumbnailMediaQuery).matches
      ? "l"
      : "s";
    return backendUrl(`/thumbnail/${id}_${size}.webm`);
  };

  let videoSrc = $state(getVideoSrc(window, id));

  $effect(() => {
    videoSrc = getVideoSrc(window, id);
  });

  const onresize = () => {
    videoSrc = getVideoSrc(window, id);
  };

  let missingThumbnailRef = $state<HTMLDivElement | null>(null);

  const on404 = () => {
    videoRef?.classList.add("hidden");
    missingThumbnailRef?.classList.remove("hidden");
  };
</script>

<svelte:window {onresize} />

<a href="/show/{id}">
  <div class="transition duration-100 ease-in-out hover:scale-[104%]">
    {#if mediaKind === "image"}
      <ThumbnailPicture prefix={`/thumbnail/${id}`} />
    {:else if mediaKind === "video"}
      <div class="grid grid-cols-1 grid-rows-1">
        <div class="col-start-1 row-start-1 h-full w-full">
          <video
            class="h-full w-full"
            src={videoSrc}
            onerror={on404}
            bind:this={videoRef}
            loop={true}
            muted={true}
            playsInline={true}
          >
          </video>
          <!-- svelte-ignore a11y_mouse_events_have_key_events -->
          <!-- svelte-ignore a11y_no_static_element_interactions -->
          <div
            class="grid hidden h-full w-full place-items-center bg-muted opacity-0"
            onmouseover={startVideo}
            onmouseleave={rewindVideo}
            bind:this={missingThumbnailRef}
          >
            <Spinner class="size-1/2" />
          </div>
        </div>
        <!-- svelte-ignore a11y_mouse_events_have_key_events -->
        <!-- svelte-ignore a11y_no_static_element_interactions -->
        <div
          class="col-start-1 row-start-1"
          onmouseover={startVideo}
          onmouseleave={rewindVideo}
          bind:this={pictureRef}
        >
          <ThumbnailPicture prefix={`/thumbnail/${id}`} />
        </div>
      </div>
    {/if}
  </div>
</a>
