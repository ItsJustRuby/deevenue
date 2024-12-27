<script lang="ts">
  import { api } from "$lib/api/client";
  import type { TagViewModel } from "$lib/api/models";
  import EditableBadgeBar from "$lib/components/editable-badge-bar.svelte";

  let { tag = $bindable() }: { tag: TagViewModel } = $props();

  const onAdd = async (impliedByThis: string) => {
    const res = await api.POST("/tag/{implying}/implications/{implied}", {
      params: {
        path: {
          implying: tag.name,
          implied: impliedByThis,
        },
      },
    });

    if (res.error) return;
    tag = res.data;
  };

  const onRemove = async (impliedByThis: string) => {
    const res = await api.DELETE("/tag/{implying}/implications/{implied}", {
      params: {
        path: {
          implying: tag.name,
          implied: impliedByThis,
        },
      },
    });

    if (res.error) return;
    tag = res.data;
  };
</script>

<EditableBadgeBar items={tag.impliedByThis} {onAdd} {onRemove} />
