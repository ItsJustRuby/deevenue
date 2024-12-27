<script lang="ts">
  import { api } from "$lib/api/client";
  import type { TagViewModel } from "$lib/api/models";
  import EditableBadgeBar from "$lib/components/editable-badge-bar.svelte";

  let { tag = $bindable() }: { tag: TagViewModel } = $props();

  const reload = async () => {
    const getResult = await api.GET("/tag/{name}", {
      params: {
        path: {
          name: tag.name,
        },
      },
    });

    if (getResult.error) return;
    tag = getResult.data;
  };

  const onAdd = async (implyingThis: string) => {
    const postResult = await api.POST(
      "/tag/{implying}/implications/{implied}",
      {
        params: {
          path: {
            implying: implyingThis,
            implied: tag.name,
          },
        },
      },
    );

    if (postResult.error) return;
    await reload();
  };

  const onRemove = async (implyingThis: string) => {
    const deleteResult = await api.DELETE(
      "/tag/{implying}/implications/{implied}",
      {
        params: {
          path: {
            implying: implyingThis,
            implied: tag.name,
          },
        },
      },
    );

    if (deleteResult.error) return;
    await reload();
  };
</script>

<EditableBadgeBar items={tag.implyingThis} {onAdd} {onRemove} />
