using Microsoft.EntityFrameworkCore;
using Domain.Models;
using Repository.Interfaces;
using Repository.Data;

namespace Repository.Implementations;

public class AppointmentRepository : GenericRepository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Appointment>> GetByPatientIdAsync(Guid patientId)
    {
        return await _dbSet
            .Where(a => a.PatientId == patientId)
            .Include(a => a.Doctor)
            .Include(a => a.Patient)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByDoctorIdAsync(Guid doctorId)
    {
        return await _dbSet
            .Where(a => a.DoctorId == doctorId)
            .Include(a => a.Doctor)
            .Include(a => a.Patient)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByDateAsync(DateTime date)
    {
        return await _dbSet
            .Where(a => a.ScheduledAt.Date == date.Date)
            .Include(a => a.Doctor)
            .Include(a => a.Patient)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync()
    {
        return await _dbSet
            .Where(a => a.ScheduledAt > DateTime.Now)
            .Include(a => a.Doctor)
            .Include(a => a.Patient)
            .ToListAsync();
    }
}
