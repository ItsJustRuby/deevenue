<script lang="ts">
  import { backendUrl, mediaQueries } from "$lib/utility";

  const { prefix }: { prefix: string } = $props();
</script>

<picture>
  <source
    srcset={backendUrl(`${prefix}_l.jpg`)}
    media={mediaQueries.largeThumbnail}
  />
  <source srcset={backendUrl(`${prefix}_s.jpg`)} media="(min-width: 640px)" />
  <img
    class="block max-h-full w-full"
    src={backendUrl(`${prefix}_l.jpg`)}
    alt=""
  />
</picture>
