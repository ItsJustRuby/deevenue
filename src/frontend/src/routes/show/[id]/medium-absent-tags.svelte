<script lang="ts">
  import type { MediumViewModel } from "$lib/api/models";
  import { api } from "$lib/api/client";
  import EditableBadgeBar from "$lib/components/editable-badge-bar.svelte";

  interface Props {
    medium: MediumViewModel;
  }

  let { medium = $bindable() }: Props = $props();

  const onAdd = async (tagName: string) => {
    const res = await api.PUT("/medium/{id}/absenttags/{tagName}", {
      params: {
        path: {
          id: medium.id,
          tagName,
        },
      },
    });
    medium = res.data!;
  };

  const onRemove = async (tagName: string) => {
    const res = await api.DELETE("/medium/{id}/absenttags/{tagName}", {
      params: {
        path: {
          id: medium.id,
          tagName,
        },
      },
    });
    medium = res.data!;
  };
</script>

<EditableBadgeBar
  getLink={(i) => `/tag/${i}`}
  items={medium.absentTags}
  {onAdd}
  {onRemove}
/>
