<script lang="ts">
  import { backendUrl, largeThumbnailMediaQuery } from "$lib/utils";

  const { prefix }: { prefix: string } = $props();
</script>

<picture>
  <source
    srcset={backendUrl(`${prefix}_l.jpg`)}
    media={largeThumbnailMediaQuery}
  />
  <source srcset={backendUrl(`${prefix}_s.jpg`)} media="(min-width: 640px)" />
  <img
    class="block max-h-full w-full"
    src={backendUrl(`${prefix}_l.jpg`)}
    alt=""
  />
</picture>
