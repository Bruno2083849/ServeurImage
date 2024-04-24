﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServeurImages.Data;
using ServeurImages.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ServeurImages.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PicturesController : ControllerBase
    {
        private readonly ServeurImagesContext _context;

        public PicturesController(ServeurImagesContext context)
        {
            _context = context;
        }

        // GET: api/Pictures
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Picture>>> GetPicture()
        {
          if (_context.Picture == null)
          {
              return NotFound();
          }
            return await _context.Picture.ToListAsync();
        }

        // GET: api/Pictures/5
        [HttpGet("{size}/{id}")]
        [DisableRequestSizeLimit]
        public async Task<ActionResult<Picture>> GetPicture(string size, int id)
        {
          if (_context.Picture == null)
          {
              return NotFound();
          }
            Picture? picture = await _context.Picture.FindAsync(id);

            if (picture == null || picture.FileName == null|| picture.mimeType == null)
            {
                return NotFound(new {Message = "Cette photo n'existe"});
            }
            if(!(Regex.Match(size, "lg|sm").Success))
            {
                return BadRequest(new { Message = "La taille demenée est inadéquate." });
            }
            byte[] bytes = System.IO.File.ReadAllBytes(Directory.GetCurrentDirectory() 
                + "/images/" + size + "/" + picture.FileName);
            return File(bytes, picture.mimeType);
        }

        
        // POST: api/Pictures
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<ActionResult<Picture>> PostPicture()
        {
          if (_context.Picture == null)
          {
              return Problem("Entity set 'ServeurImagesContext.Picture'  is null.");
          }
            

          try
          {

                IFormCollection formCollection = await Request.ReadFormAsync();
                IFormFile? file = formCollection.Files.GetFile("monImage");
                if(file != null)
                {
                    Image image = Image.Load(file.OpenReadStream());

                    Picture picture = new Picture()
                    {
                        Id = 0,
                        FileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName),
                        mimeType = file.ContentType
                    };

                    image.Save(Directory.GetCurrentDirectory() + "/images/lg/" + picture.FileName);
                    image.Mutate(i =>

                        i.Resize(new ResizeOptions()
                        {
                            Mode = ResizeMode.Min,
                            Size = new Size() { Width = 320}
                        }
                        
                        )
                    );
                    image.Save(Directory.GetCurrentDirectory() + "/images/sm/" + picture.FileName);
                    _context.Picture.Add(picture);
                    await _context.SaveChangesAsync();
                }
            }
          catch (Exception)
            {
                return NotFound(new { Message = "Aucun image fournie" });
            }
            
           
            return Ok();
        }

        // DELETE: api/Pictures/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePicture(int id)
        {
            if (_context.Picture == null)
            {
                return NotFound();
            }
            var picture = await _context.Picture.FindAsync(id);
            if (picture == null)
            {
                return NotFound();
            }

            _context.Picture.Remove(picture);
            await _context.SaveChangesAsync();

            return NoContent();
        }

      
    }
}
