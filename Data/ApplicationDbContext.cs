using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineTicket.Models;

namespace OnlineTicket.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Organizer> Organizers { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<TicketType> TicketTypes { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Promotion> Promotions { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Venue> Venues { get; set; }
    public DbSet<Customer> Customers { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Organizer <-> IdentityUser (1:1)
        builder.Entity<Organizer>()
            .HasOne(o => o.User)
            .WithOne()
            .HasForeignKey<Organizer>(o => o.IdentityUserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        // Organizer <-> IdentityUser (1:1)
        builder.Entity<Organizer>()
                .HasMany(o => o.Events)
                .WithOne(e => e.Organizer)
                .HasForeignKey(e => e.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);

        // Customer <-> IdentityUser (1:1)
        builder.Entity<Customer>()
            .HasOne(c => c.User)
            .WithOne()
            .HasForeignKey<Customer>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Customer <-> booking (1:N)
        builder.Entity<Booking>()
            .HasOne(o => o.Customer)
            .WithMany(c => c.Bookings)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Category -> Events
        builder.Entity<Category>()
            .HasMany(c => c.Events)
            .WithOne(e => e.Category)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Venue -> Events
        builder.Entity<Venue>()
            .HasMany(v => v.Events)
            .WithOne(e => e.Venue)
            .HasForeignKey(e => e.VenueId)
            .OnDelete(DeleteBehavior.Restrict);

        // Event -> TicketTypes
        builder.Entity<Event>()
            .HasMany(e => e.TicketTypes)
            .WithOne(tt => tt.Event)
            .HasForeignKey(tt => tt.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        // Event -> Bookings
        builder.Entity<Event>()
            .HasMany(e => e.Bookings)
            .WithOne(b => b.Event)
            .HasForeignKey(b => b.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        // Event -> Tickets
        builder.Entity<Event>()
            .HasMany(e => e.Tickets)
            .WithOne(t => t.Event)
            .HasForeignKey(t => t.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        // Booking -> Tickets
        builder.Entity<Booking>()
            .HasMany(b => b.Tickets)
            .WithOne(t => t.Booking)
            .HasForeignKey(t => t.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Booking -> Payment
        builder.Entity<Booking>()
            .HasOne(b => b.Payment)
            .WithOne(p => p.Booking)
            .HasForeignKey<Payment>(p => p.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Promotion>()
             .HasOne(p => p.Event)
             .WithMany(e => e.Promotions)
             .HasForeignKey(p => p.EventId)
             .OnDelete(DeleteBehavior.Restrict); // prevents multiple cascade paths

        builder.Entity<Promotion>()
            .HasOne(p => p.TicketType)
            .WithMany()
            .HasForeignKey(p => p.TicketTypeId)
            .OnDelete(DeleteBehavior.Restrict); // prevents multiple cascade paths


        // Optional: Event default status
        builder.Entity<Event>()
            .Property(e => e.Status)
            .HasDefaultValue("Active");
    }


}
