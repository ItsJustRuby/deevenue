<script lang="ts">
  import { goto } from "$app/navigation";
  import { api } from "$lib/api/client";
  import * as AlertDialog from "$lib/components/ui/alert-dialog/index.js";
  import { Button } from "$lib/components/ui/button/index.js";
  import Delete from "$lib/components/Delete";

  const { id }: { id: string } = $props();

  const deleteMedium = async () => {
    await api.DELETE("/medium/{id}", {
      params: {
        path: { id },
      },
    });

    await goto("/");
  };
</script>

<AlertDialog.Root>
  <AlertDialog.Trigger>
    <Button variant="destructive"><Delete /></Button>
  </AlertDialog.Trigger>
  <AlertDialog.Content>
    <AlertDialog.Header>
      <AlertDialog.Title>Confirmation</AlertDialog.Title>
    </AlertDialog.Header>
    <AlertDialog.Description>
      Do you really want to delete this medium?
    </AlertDialog.Description>
    <AlertDialog.Footer>
      <AlertDialog.Cancel>Cancel</AlertDialog.Cancel>
      <AlertDialog.Action class="bg-destructive" onclick={deleteMedium}>
        Delete
      </AlertDialog.Action>
    </AlertDialog.Footer>
  </AlertDialog.Content>
</AlertDialog.Root>
