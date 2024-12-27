<script lang="ts">
  import { api } from "$lib/api/client";
  import type { TagViewModel } from "$lib/api/models";
  import EditableBadgeBar from "$lib/components/editable-badge-bar.svelte";

  let { tag = $bindable() }: { tag: TagViewModel } = $props();

  const onAdd = async (alias: string) => {
    const res = await api.POST("/tag/{tagName}/aliases/{alias}", {
      params: {
        path: {
          tagName: tag!.name,
          alias,
        },
      },
    });

    if (res.error) return;
    tag = res.data;
  };

  const onRemove = async (alias: string) => {
    const res = await api.DELETE("/tag/{tagName}/aliases/{alias}", {
      params: {
        path: {
          tagName: tag!.name,
          alias,
        },
      },
    });

    if (res.error) return;
    tag = res.data;
  };
</script>

<EditableBadgeBar items={tag.aliases} {onAdd} {onRemove} />
