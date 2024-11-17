<script lang="ts">
  import Search from "lucide-svelte/icons/search";

  import { goto } from "$app/navigation";
  import { page } from "$app/stores";
  import TagTitle from "./tag-title.svelte";
  import { api } from "$lib/api/client";
  import type { TagViewModel } from "$lib/api/models";
  import { onMount } from "svelte";
  import TagAliases from "./tag-aliases.svelte";
  import TagImplyingThis from "./tag-implying-this.svelte";
  import TagImpliedByThis from "./tag-implied-by-this.svelte";

  let tag = $state<TagViewModel | null>(null);

  onMount(async () => {
    const currentName = $page.params.name!;
    const res = await api.GET("/tag/{name}", {
      params: {
        path: {
          name: currentName,
        },
      },
    });

    if (!res.error) {
      tag = res.data;

      if (tag.name !== currentName)
        goto(`/tag/${tag.name}`, { replaceState: true });

      return;
    }

    if (res.response.status === 404) await goto("/");
  });
</script>

<div>
  {#if tag !== null}
    <div class="flex flex-row items-baseline">
      <TagTitle bind:tag />
      <div>
        <a href="/search/{tag.name}"><Search /></a>
      </div>
    </div>
    <div class="pt-2">Used {tag.mediaCount} times</div>
    <div>
      Aliases:
      <div>
        <TagAliases bind:tag />
      </div>
    </div>
    <div>
      Implying this:
      <div>
        <TagImplyingThis bind:tag />
      </div>
    </div>
    <div>
      Implied by this:
      <div>
        <TagImpliedByThis bind:tag />
      </div>
    </div>
  {/if}
</div>
