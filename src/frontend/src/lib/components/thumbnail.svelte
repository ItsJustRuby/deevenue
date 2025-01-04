<script lang="ts">
  import type { MediumViewModel } from "$lib/api/models";
  import { backendUrl, mediaQueries } from "$lib/utility";
  import { MediaQuery } from "svelte/reactivity";
  import Spinner from "./spinner.svelte";
  import ThumbnailPicture from "./thumbnail-picture.svelte";

  const { id, mediaKind }: Pick<MediumViewModel, "id" | "mediaKind"> = $props();

  let videoRef = $state<HTMLVideoElement | undefined>(undefined);
  let pictureRef = $state<HTMLDivElement | undefined>(undefined);

  const startVideo = async () => {
    missingThumbnailRef?.classList.remove("opacity-0");
    try {
      await videoRef!.play();
      // eslint-disable-next-line @typescript-eslint/no-unused-vars
    } catch (e: unknown) {
      // Explicitly fine if the video src is 404.
    }
    // Only do this after awaiting the play() - that initially loads the
    // video's src. If you make the pictureRef invisible before then,
    // you get a (white) flash of the background shining through.
    pictureRef!.classList.add("opacity-0");
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

  var mediaQuery = new MediaQuery(mediaQueries.largeThumbnail);
  let videoSrc = $derived(
    backendUrl(`/thumbnail/${id}_${mediaQuery.current ? "l" : "s"}.webm`),
  );
  let imgSrc = $derived(
    backendUrl(`/thumbnail/${id}_${mediaQuery.current ? "l" : "s"}.jpg`),
  );

  let missingThumbnailRef = $state<HTMLDivElement | null>(null);

  const on404 = () => {
    missingThumbnailRef?.classList.remove("hidden");
    videoRef?.classList.add("hidden");
  };
</script>

<a href="/show/{id}">
  <div
    class="select-none transition duration-100 ease-in-out hover:scale-[104%]"
  >
    {#if mediaKind === "image"}
      <ThumbnailPicture prefix={`/thumbnail/${id}`} />
    {:else if mediaKind === "video"}
      <div class="grid grid-cols-1 grid-rows-1">
        <div class="col-start-1 row-start-1 h-full w-full">
          <!--
          preload="none" is strongly encouraged:
          * preload="auto" would preload all video thumbnails on screen, regardless
            whether the user can even see them (e.g. if they are on mobile).
          * not specifying preload at all makes the browser decide what it wants,
            and it is often an idiot about prioritizing media requests. 
          
          However, by not preloading the video src, we *do* need to preload the `poster`:
          Without that, the browser does not know what height this div should have
          (width is determined by the containing column).
          -->
          <video
            class="h-full w-full"
            src={videoSrc}
            onerror={on404}
            bind:this={videoRef}
            loop={true}
            muted={true}
            playsInline={true}
            preload="none"
            poster={imgSrc}
            disablepictureinpicture
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
