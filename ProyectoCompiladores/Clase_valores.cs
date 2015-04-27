using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoCompiladores
{
    class Clase_valores
    {     
        string mod;
        string id;
        string extend;
        List<Variable> variables; //de variables deseamos saber su ID y su tipo y si es const tambien el mod
        List<Metodo> metodos; //de los metodos requerimos saber su mod su valor de retorno y sus atriburtos (en orden)
        public Clase_valores ()
        {
            mod = "protected";
            extend = "";
            variables = new List<Variable>();
            metodos = new List<Metodo>();
        }
        public string Id 
        {
            get { return id; }
            set { id = value; }             
        }
        public string Mod 
        { 
            get {return mod ;} 
            set{mod = value;} 
        }
        public string Extend
        {
            get { return extend; }
            set { extend = value; }
        }
        public List<Variable> Variables
        {
            get { return variables; }
            //set { variables = value; }
        }
        public List<Metodo> Metodos
        {
            get { return metodos; }
            //set { metodos = value; }
        }
        
    }
    class Variable
    {
        string mod;
        string id;
        string tipo;
        bool is_const;
        string valor;
        public Variable()
        {
            mod = "private";
            is_const = false;
            valor = "";
        }
        public string Id
        {
            get { return id; }
            set { id = value; }
        }
        public string Mod
        {
            get { return mod; }
            set { mod = value; }
        }
        public string Tipo
        {
            get { return tipo; }
            set { tipo = value; }
        }
        public void asigna_const(string _valor)
        {
            is_const = true;
            valor = _valor;
        }
        public void reset()
        {
            mod = "private";
            is_const = false;
            valor = "";
        }
    }
    // clase que almacena las propiedades de los metodos
    class Metodo
    {
        string mod;
        string id;
        string tipo;
        List<string> tiposFirma;
        List<string> idTIpos_firma;
        public Metodo()
        {
            mod = "public";
            tiposFirma = new List<string>();
        }
        //getters y setters
        public string Id
        {
            get { return id; }
            set { id = value; }
        }
        public string Mod
        {
            get { return mod; }
            set { mod = value; }
        }
        public string Tipo
        {
            get { return tipo; }
            set { tipo = value; }
        }
        public List<string> TiposFirma
        { 
            get { return tiposFirma; }
            set { tiposFirma = value; }
        }
        public List<string> idTipos_firma
        {
            get { return idTipos_firma; }
            set { idTipos_firma = value; }
        }
        public void reset()
        {
            mod = "public";
            tiposFirma.Clear();
        }
        

    }

}
