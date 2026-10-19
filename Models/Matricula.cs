using System;

namespace AcademiaCentralApi.Models;

    public class Matricula
    {
        public int Id { get; set; }
        public int AlumnoId { get; set; }
        public Alumno Alumno { get; set; } = null!;
        public int CarreraId { get; set; }
        public Carrera Carrera { get; set; } = null!;

        public string PeriodoAcademico { get; set; } = string.Empty;
        public DateTime FechaInicioVigencia { get; set; }
        public DateTime FechaFinVigencia { get; set; }
        public EstadoMatricula Estado { get; set; }

        protected Matricula() { }

        public Matricula(int alumnoId, int carreraId, string periodoAcademico, DateTime fechaInicioVigencia, DateTime fechaFinVigencia, EstadoMatricula estado)
        {
            AlumnoId = alumnoId;
            CarreraId = carreraId;
            PeriodoAcademico = periodoAcademico;
            FechaInicioVigencia = fechaInicioVigencia;
            FechaFinVigencia = fechaFinVigencia;
            Estado = estado;
        }
    }
