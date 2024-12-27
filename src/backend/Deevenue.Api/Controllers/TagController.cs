using Deevenue.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Deevenue.Api.Controllers;

public class TagController(ITagRepository repository) : DeevenueApiControllerBase
{
    [HttpGet("", Name = "getAllTags")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<AllTagsViewModel>> GetAll()
    {
        return await repository.GetAllAsync();
    }

    [HttpGet("{name}", Name = "getTag")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TagViewModel>> Get(string name)
    {
        var maybeTag = await repository.FindAsync(name);

        if (maybeTag == null)
            return NotFound();

        return maybeTag;
    }

    [HttpPatch("{currentName}/name/{newName}", Name = "renameTag")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TagViewModel>> Rename(string currentName, string newName)
    {
        var ok = await repository.RenameAsync(currentName, newName);
        if (!ok)
            return NotFound();
        return await Get(newName);
    }

    [HttpPut("{tagName}/rating/{rating:Rating}", Name = "setTagRating")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TagViewModel>> SetRating(string tagName, Rating rating)
    {
        var ok = await repository.SetRatingAsync(tagName, rating);
        if (!ok)
            return NotFound();
        return await Get(tagName);
    }


    [HttpPost("{tagName}/aliases/{alias}", Name = "addTagAlias")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TagViewModel>> AddAlias(string tagName, string alias)
    {
        var ok = await repository.AddAliasAsync(tagName, alias);
        if (!ok)
            return NotFound();

        return await Get(tagName);
    }

    [HttpDelete("{tagName}/aliases/{alias}", Name = "removeTagAlias")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TagViewModel>> RemoveAlias(string tagName, string alias)
    {
        var ok = await repository.RemoveAliasAsync(tagName, alias);
        if (!ok)
            return NotFound();
        return await Get(tagName);
    }


    [HttpPost("{implying}/implications/{implied}", Name = "addImplication")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TagViewModel>> AddImplication(string implying, string implied)
    {
        var ok = await repository.AddImplicationAsync(implying, implied);
        if (!ok)
            return NotFound();

        return await Get(implying);
    }

    [HttpDelete("{implying}/implications/{implied}", Name = "removeImplication")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TagViewModel>> RemoveImplication(string implying, string implied)
    {
        var ok = await repository.RemoveImplicationAsync(implying, implied);
        if (!ok)
            return NotFound();
        return await Get(implying);
    }
}
