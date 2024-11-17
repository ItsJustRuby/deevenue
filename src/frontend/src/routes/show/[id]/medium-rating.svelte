<script lang="ts">
  import { goto } from "$app/navigation";
  import { api } from "$lib/api/client";
  import type { MediumViewModel, Rating } from "$lib/api/models";
  import { Label } from "$lib/components/ui/label/index.js";
  import * as RadioGroup from "$lib/components/ui/radio-group/index.js";
  import * as Card from "$lib/components/ui/card/index.js";

  interface Props {
    medium: MediumViewModel;
  }

  let { medium = $bindable() }: Props = $props();

  let rating = $state(medium.rating);

  const setRating = async (r: Rating) => {
    const res = await api.PUT("/medium/{id}/rating/{rating}", {
      params: {
        path: {
          id: medium.id,
          rating: r,
        },
      },
    });

    if (res.error) {
      if (res.response.status === 403) {
        await goto("/");
      }
      return;
    }

    medium = res.data;
  };
</script>

<Card.Root>
  <Card.Content>
    <RadioGroup.Root
      class="min-w-32 grid-rows-3 gap-4"
      value={rating}
      onValueChange={(e) => setRating(e as Rating)}
    >
      <div class="flex items-center space-x-2">
        <RadioGroup.Item value="safe" id="s" />
        <Label for="safe">s</Label>
      </div>
      <div class="flex items-center space-x-2">
        <RadioGroup.Item value="questionable" id="q" />
        <Label for="questionable">q</Label>
      </div>
      <div class="flex items-center space-x-2">
        <RadioGroup.Item value="explicit" id="e" />
        <Label for="explicit">e</Label>
      </div>
    </RadioGroup.Root>
  </Card.Content>
</Card.Root>
