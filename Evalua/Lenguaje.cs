using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
namespace Evalua
{
    /* 
    Requerimiento 1: Programar el residuo de la division en C# y asm (DX) ✔✔
    Requerimiento 2: Programar en ensamblador el scanf ✔✔
    Requerimiento 3: Programar el >,>=,<,<= en asm ✔✔
    Requerimiento 4: Programar el while en asm
    Requerimiento 5: Programar el else en asm 
    */
    public class Lenguaje:Sintaxis
    {
        int contadorIF=0;
        int contadorFor=0;
        int contadorWhile=0;
        int contadorELSE=0;
        List<Variable> LV; //declaracion 
        Stack<float> SE;
        List <string > LS;
        Variable.TDatos Dominante; //Si se encuentra un float, char, int en una expresion matematica
        public Lenguaje()
        {
            LV=new List<Variable>(); //instancio el objeto
            SE = new Stack<float>(); //instancia del objeto stack
            LS = new List<string>();
        }
        // Programa	-> 	Librerias Variables Main
        public void Programa()
        {
            Librerias();
            Variables();
            asm.WriteLine("include \"emu8086.inc\"");
            asm.WriteLine("ORG 100h");
            Main();
            asm.WriteLine("ret");
            asm.WriteLine("define_print_string");
            asm.WriteLine("define_print_num");
            asm.WriteLine("define_print_num_uns");
            asm.WriteLine("define_scan_num");
            ImprimeLista();            
        }
        // Librerias->	#include<identificador(.h)?> Librerias?
        private void Librerias()
        {
            Match("#");
            Match("include");
            Match("<");
            Match(Tipos.identificador);
            if(getContenido()==".")
            {
                Match(".");
                Match("h");
            }
            Match(">");
            if(getContenido()=="#")
            {
                Librerias();
            }
        }
        private Variable.TDatos StrignToEnum(string tipo)
        {
            switch(tipo)
            {
                case "char": return Variable.TDatos.CHAR;
                case "int": return Variable.TDatos.INT;
                case "float": return Variable.TDatos.FLOAT;
                default: return Variable.TDatos.sinTipo;
            }
        }
        //Variables ->  tipoDato ListaIdentificadores; Variables?
        private void Variables()
        {
            Variable.TDatos tipo=Variable.TDatos.CHAR; //inicializamos=que cero
            tipo=StrignToEnum(getContenido());
            Match(Tipos.tipoDato);
            ListaIdentificadores(tipo);
            Match(Tipos.finSentencia);
            if(getClasificacion()==Tipos.tipoDato) //recursividad de Variables
            {
                Variables();
            }
        }
        private void ImprimeLista()
        {
            log.WriteLine("Lista de Variables");
            asm.WriteLine("; variables");
            foreach (Variable L in LV)
            {
                log.WriteLine(L.getNombre() + " " + L.getTipoDato() + " "+ L.getValor());
                asm.Write(L.getNombre() + " ");
                switch (L.getTipoDato())
                {
                    case Variable.TDatos.CHAR: asm.WriteLine("db 0"); break;
                    case Variable.TDatos.INT: asm.WriteLine("dw 0"); break;
                    case Variable.TDatos.FLOAT: asm.WriteLine("dd 0"); break;
                }
            }
        }
        private bool Existe(string nombre)
        {
            foreach (Variable L in LV)
            {
                if(L.getNombre()== nombre)
                {
                    return true;
                }    
            }
            return false; //no existe la variable
        }
        private void Modifica(string nombre, float valor)
        {
            foreach (Variable L in LV)
            {
                if(L.getNombre()== nombre)
                {
                    L.setValor(valor); 
                }    
            }
        }
        private float GetValor(string nombre)
        {
            foreach (Variable L in LV)
            {
                if(L.getNombre()== nombre)
                {
                    return L.getValor(); 
                }    
            }
            return 0;
        }
        private Variable.TDatos GetTipo(string nombre)
        {
            foreach (Variable L in LV)
            {
                if(L.getNombre()== nombre)
                {
                    return L.getTipoDato(); 
                }    
            }
            return Variable.TDatos.sinTipo;
        }
        //ListaIdentificadores ->  identificador (,ListaIdentificadores)?
        private void ListaIdentificadores(Variable.TDatos tipo)
        {
            if(tipo != Variable.TDatos.sinTipo)
            {
                if(!Existe(getContenido())) 
                {
                    LV.Add(new Variable(getContenido(), tipo));   
                }
                else 
                {
                    throw new Error("ERROR DE SINTAXIS: Variable duplicada: " + getContenido(),linea,log);
                }    
            }
            else
            {

                LS.Add(GetValor(getContenido()).ToString());
            }
            Match(Tipos.identificador);
            if(getContenido()==",")//recursividad de lista
            {
                Match(",");
                ListaIdentificadores(tipo);
            }
        }
        // Main  ->	void main() BloqueInstrucciones
        private void Main()
        {
            Match("void");
            Match("main");
            Match("(");
            Match(")");
            BloqueInstrucciones(true);
        }
        // BloqueInstrucciones ->  {ListaInstrucciones}
        private void BloqueInstrucciones(bool ejecuta)
        {
            Match("{");
            ListaInstrucciones(ejecuta);
            Match("}");
        }
        // ListaInstrucciones -> Instruccion ListaInstrucciones?
        private void ListaInstrucciones(bool ejecuta)
        {
            Instruccion(ejecuta);
            if(getContenido() != "}") 
            {
                ListaInstrucciones(ejecuta);
            }
    
        }
        // Instrccion  -> Printf  | Scanf | If | For | While | Switch | Asignacion 
        private void Instruccion(bool ejecuta)
        {
            if(getContenido() == "printf")
            {
                Printf(ejecuta);
            }
            else if(getContenido() == "scanf")
            {
                Scanf(ejecuta);
            }
            else if(getContenido() == "if")
            {
                If(ejecuta);
            }
            else if(getContenido() == "for")
            {
                For(ejecuta);
            }
            else if(getContenido() == "while")
            {
                While(ejecuta);
            }
            else if(getContenido() == "switch")
            {
                Switch(ejecuta);
            }
            else if(getContenido() == "do")
            {
                Do(ejecuta);
            }
            else
            {
                Asignacion(ejecuta);
            }
        }
        // Printf -> printf(cadena (,ListaIdentificadores)?);
        private void Printf(bool ejecuta)
        {
            Match("printf");
            Match("(");
            string quitar = getContenido();
            var regex = new Regex("%f");
            quitar = quitar.Replace("\"","").Replace("\\t","\t").Replace("\\n", "\n");            
            Match(Tipos.Cadena);
            if(getContenido() == ",")
            {          
                Match(",");
                LS.Clear();
                ListaIdentificadores(Variable.TDatos.sinTipo);
                foreach(string L in LS)
                {
                    quitar = regex.Replace(quitar, L, 1);    
                }
            }
            Match(")");
            Match(Tipos.finSentencia);
            asm.WriteLine("PRINT " +"\"" +quitar + "\"");
            if(ejecuta)
            {
                Console.WriteLine(quitar);
            }
        } 
        // Scanf -> scanf(cadena,ListadeAmpersas);
        private void Scanf(bool ejecuta)
        {
            Match("scanf");
            Match("(");
            Match(Tipos.Cadena);
            Match(",");
            ListadeAmpersas(ejecuta);
            Match(")");
            Match(Tipos.finSentencia);
        } 
        // ListadeAmpersas -> & identificador(,ListadeAmpersas)?
        private void ListadeAmpersas(bool ejecuta)
        {
            Match("&");
            if(!Existe(getContenido())) //buscarlos en Match(Tipos.identificador) y agregar excepcion
            {
                throw new Error("ERROR DE SINTAXIS: Variable no declarada: " + getContenido(),linea,log);
            }
            asm.WriteLine("CALL SCAN_NUM");
            asm.WriteLine("MOV "+ getContenido() + ",CX");
            if(ejecuta)
            {
                float valor=float.Parse(Console.ReadLine());//lo asigna 
                Modifica(getContenido(),valor);
            } 
            Match(Tipos.identificador);
            if(getContenido()==",")
            {
                Match(",");
                ListadeAmpersas(ejecuta);
            }
        }
        // If -> if(Condicion) BloqueInstrucciones | Intruccion (else BloqueInstrcciones | Instruccion)?
        private void If(bool ejecuta)
        {
            string etiqueta = "EIF"+contadorIF++;
            Match("if");
            Match("(");
            bool evalua=Condicion(etiqueta);//si es verdadero o falsa
            Match(")");
            if(getContenido()!="{")
            {
                Instruccion(evalua && ejecuta);
            }
            else
            {
                BloqueInstrucciones(evalua && ejecuta);
            }
            //? 
            if(getContenido()=="else")
            {
                string etiqueta2 = "EELSE"+contadorELSE++;
                Match("else");
                asm.WriteLine("JMP "+ etiqueta2);
                asm.WriteLine(etiqueta+":");
                if(getContenido()!="{")
                {
                    Instruccion(!evalua && ejecuta);
                }
                else
                {
                    BloqueInstrucciones(!evalua && ejecuta);
                }
                asm.WriteLine(etiqueta2+":");
                return;
            }
            asm.WriteLine(etiqueta+":");
        }
        // ***Condicion -> Expresion oprRelacional Expresion  
        private bool Condicion(string etiqueta)
        {
            //considerando la negacion 
            bool neg = false;
            if(getContenido()=="!")
            {
                Match("!");
                neg = true;
            }
            Expresion();
            string operador = getContenido();
            Match(Tipos.opRelacional);
            Expresion();
            float Resultado2 = SE.Pop();
            asm.WriteLine("POP CX");
            float Resultado1 = SE.Pop();
            asm.WriteLine("POP BX");
            asm.WriteLine("CMP CX, BX"); //Comparamos en asm
            if(neg) //Considerar el not(!)
            {
                switch(operador)
                {
                    case "==":  asm.WriteLine("JE "+etiqueta);
                                return Resultado1 != Resultado2; 
                    case ">=":  asm.WriteLine("JLE "+etiqueta);
                                return Resultado1 <=Resultado2; 
                    case ">":   asm.WriteLine("JL "+etiqueta);
                                return Resultado1 < Resultado2; 
                    case "<=":  asm.WriteLine("JGE "+etiqueta);
                                return Resultado1 >=  Resultado2; 
                    case "<":   asm.WriteLine("JG "+etiqueta);
                                return Resultado1 > Resultado2; 
                    default:    asm.WriteLine("JNE "+etiqueta);
                                return Resultado1 == Resultado2; 
                }
            }
            else
            {
                switch(operador)
                {
                    case "==":  asm.WriteLine("JNE "+etiqueta);
                                return Resultado1 == Resultado2; 
                    case ">=":  asm.WriteLine("JGE "+etiqueta);
                                return Resultado1 >= Resultado2; 
                    case ">":   asm.WriteLine("JG "+etiqueta);
                                return Resultado1 > Resultado2; 
                    case "<=":  asm.WriteLine("JLE "+etiqueta);
                                return Resultado1 <= Resultado2; 
                    case "<":   asm.WriteLine("JL "+etiqueta);
                                return Resultado1 < Resultado2; 
                    default:    asm.WriteLine("JE "+etiqueta);
                                return Resultado1 != Resultado2; 
                }
            }
        }
        // ***For -> for(identificador=Expresion; Condicion; identificador incTermino) BloqueInstrucciones | Instruccion
        private void For(bool ejecuta)
        {
            string EBeginFor="EBEGINFOR"+contadorFor++;
            string EEndFor="EEndFOR"+contadorFor;
            Match("for");
            Match("(");
            if(!Existe(getContenido())) 
            {
                throw new Error("ERROR DE SINTAXIS: Variable no declarada: " + getContenido(),linea,log);
            }
        
            string variable=getContenido();
            if(!Existe(getContenido())) 
            {
                throw new Error("ERROR DE SINTAXIS: Variable no declarada: " + getContenido(),linea,log);
            }
            Match(Tipos.identificador);
            Match("=");
            Expresion();
            float Resultado=SE.Pop(); //variable del resultado de operacion
            asm.WriteLine("POP AX");
            Modifica(variable,Resultado); 
            asm.WriteLine("MOV "+ variable +", AX"); 
            Match(Tipos.finSentencia);
            asm.WriteLine(EBeginFor+":"); //etiqueta de incio 
            bool evalua=Condicion(EEndFor);
            Match(Tipos.finSentencia);
            string variable2=getContenido();
            if(!Existe(getContenido())) 
            {
                throw new Error("ERROR DE SINTAXIS: Variable no declarada: " + getContenido(),linea,log);
            }
            Match(Tipos.identificador); 
            string operador=getContenido();
            Match(Tipos.incTermino);
            if(operador=="++")
            {
                Modifica(variable2,GetValor(variable2)+1);
            }
            else if(operador=="--")
            {
                Modifica(variable2,GetValor(variable2)-1);
            }
            Match(")");
            if(getContenido()!="{")
            {
                Instruccion(evalua);
            }
            else
            {
                BloqueInstrucciones(evalua);
            }
            //aquiva el decremento y aumento
            if(operador=="++")
            {
                asm.WriteLine("INC "+variable2);
            }
            else if(operador=="--")
            {
                asm.WriteLine("DEC "+variable2);
            }
            asm.WriteLine("JMP "+EBeginFor);
            asm.WriteLine(EEndFor +":"); //cuando termina
        }
        private void Do(bool ejecuta)
        {
            bool evalua = true;
            Match("do");
            BloqueInstrucciones(evalua);
            Match("while");
            Match("(");
            evalua = Condicion("");
            Match(")");
            Match(";");
        }
        // While -> while(Condicion) BloqueInstrucciones | Instruccion
        private void While(bool ejecuta)
        {
            string EBeginWhile = "BEGINWHILE"+contadorWhile++;
            string EEndWhile ="ENDWHILE"+ contadorWhile;
            Match("while");
            Match("(");
            asm.WriteLine(EBeginWhile+":"); //etiqueta de incio 
            bool evalua=Condicion(EEndWhile);
            Match(")");
            if(getContenido()!="{") // bloque de instrucciones y instrucciones
            {
                Instruccion(evalua);
            }
            else
            {
                BloqueInstrucciones(evalua);
            }
            asm.WriteLine("JMP "+EBeginWhile);
            asm.WriteLine(EEndWhile +":"); //cuando termina
        }
        // Switch -> switch(Expresion)  {VariosCase Default}
        private void Switch(bool ejecuta)
        {
            Match("switch");
            Match("("); 
            Expresion();
            float Resultado = SE.Pop();
            asm.WriteLine("POP");
            Match(")");
            Match("{");
            VariosCase();
            Default();
            Match("}");
        }
        // VariosCase -> case numero: VariosCase | CaseInstruccion
        private void VariosCase()
        {
            Match("case");  
            Match(Tipos.numero);
            Match(":");
            if(getContenido() != "case")
            {
                CaseInstruccion();
            }
            else
            {
                VariosCase();
            } 
        }
        // CaseInstruccion  -> Instruccion| BloqueInstrucciones Break 
        private void CaseInstruccion()
        {
            if(getContenido()!="{")
            {
                Instruccion(true);
            }
            else
            {
                BloqueInstrucciones(true);
            }
            Break();
        }
        //Break -> break;? VariosCase
        private void Break()
        {
            if(getContenido() == "break")
            {
                Match("break");
                Match(Tipos.finSentencia);
                if(getContenido() == "case")
                {
                    VariosCase();
                }
            }
        } 
        // Default-> default:? Intruccion | BloqueInstrcciones 
        private void Default()
        {
            if(getContenido()=="default")
            {
                Match("default");
                Match(":");
                if(getContenido()!="{")
                {
                    Instruccion(true);
                }
                else
                {
                    BloqueInstrucciones(true);
                }
            }   
        }
        // Asignacion  -> identificador = Expresion;
        private void Asignacion(bool ejecuta)
        {
            Dominante = Variable.TDatos.sinTipo; //tiene el valor mas bajo(0)
            string variable =getContenido();
            if(!Existe(getContenido())) //buscarlos en Match(Tipos.identificador) y agregar excepcion
            {
                throw new Error("ERROR DE SINTAXIS: Variable no declarada: " + getContenido(),linea,log);
            }
            Match(Tipos.identificador);
            if(getContenido()=="++" || getContenido()=="--")
            {
                string operador=getContenido();
                Match(Tipos.incTermino);
                if(operador=="++")
                {
                    Modifica(variable,GetValor(variable)+1);
                    asm.WriteLine("INC "+variable);
                }
                else if(operador=="--")
                {
                    Modifica(variable,GetValor(variable)-1);
                    asm.WriteLine("DEC "+variable);
                }
                Match(Tipos.finSentencia);
            }
            else
            {
                Match(Tipos.asignacion);
                Expresion();
                float Resultado = SE.Pop(); //Variable del resultado de la operacion
                asm.WriteLine("POP AX");
                asm.WriteLine("MOV " + variable + ", AX");
                //Resultado %= 256; //se asume que la variable es de tipo char 
                //Dominante = Variable.TDatos.CHAR; //Esto sedebe de hacer para cubrir el requerimiento 4
                //Console.WriteLine(GetTipo(variable) + " " + ValorToEnum(Resultado) + " " + Dominante);
                if(Dominante < ValorToEnum(Resultado))
                {
                    Dominante = ValorToEnum(Resultado);
                }
                //Console.WriteLine(GetTipo(variable) + " " + ValorToEnum(Resultado) + " " + Dominante);
                if(Dominante<=GetTipo(variable))
                {
                    Modifica(variable,Resultado);
                }
                else
                {
                    throw new Error("ERROR DE SEMANTICA: La variable: " + variable + " es de tipo " +GetTipo(variable) + " y se esta asignando un " + Dominante, linea,log);
                }
                Match(Tipos.finSentencia);
            }
        }
        private Variable.TDatos ValorToEnum(float valor)
        {
            if(valor%1 !=0)
            {
                return Variable.TDatos.FLOAT;
            }
            else if(valor < 256)
            {
                return Variable.TDatos.CHAR;
            }
            else if (valor < 65535)
            {
                return Variable.TDatos.INT;
            }
            return Variable.TDatos.FLOAT;
        }
        // Expresion  -> Termino MasTermino 
        private void Expresion()
        {
            Termino();
            MasTermino();
        }
        // MasTermino -> (opTermino Termino)? 
        private void MasTermino()
        {
            if(getClasificacion()==Tipos.opTermino)
            {
                string operador=getContenido();
                Match(Tipos.opTermino);
                Termino();
                //Console.Write(operador + " ");//segundo termino
                float N1 = SE.Pop(); //sacamos un elemento
                asm.WriteLine("POP BX");
                float N2 = SE.Pop();//sacamos un segundo elemento
                asm.WriteLine("POP AX");
                switch(operador)
                {
                    case "+":   SE.Push(N2+N1);
                                asm.WriteLine("ADD AX, BX");
                                asm.WriteLine("PUSH AX");
                                break;
                    case "-":   SE.Push(N2-N1);
                                asm.WriteLine("SUB AX, BX");
                                asm.WriteLine("PUSH AX");
                                break;
                }
            }
        }
        // Termino	  -> Factor PorFactor 
        private void Termino()
        {
            Factor();
            PorFactor();
        }
        // PorFactor  -> (opFactor Factor)?
        private void PorFactor()
        {
            if(getClasificacion()==Tipos.opFactor)
            {
                string operador=getContenido();
                Match(Tipos.opFactor);
                Factor();
                //Console.Write(operador + " ");//primer termino
                float N1 = SE.Pop(); //sacamos un elemento
                asm.WriteLine("POP BX");
                float N2 = SE.Pop();//sacamos un segundo elemento
                asm.WriteLine("POP AX");
                switch(operador)
                {
                    case "*":   SE.Push(N2*N1);
                                asm.WriteLine("MUL BX");
                                asm.WriteLine("PUSH AX");
                                break;
                    case "/":   SE.Push(N2/N1);
                                asm.WriteLine("DIV BX");
                                asm.WriteLine("PUSH AX");
                                break;
                    case "%":   SE.Push(N2%N1);
                                asm.WriteLine("DIV BX");
                                asm.WriteLine("PUSH DX");
                                break;
                }
            }
        }
        private float ValorCasteado(float N1, Variable.TDatos Cas)
        {
            if(Cas == Variable.TDatos.CHAR)
            {
                return N1 % 256;
            }
            else if(Cas == Variable.TDatos.INT)
            {
                return N1 % 65536;
            }
            return N1;
        }
        // Factor	  -> numero | identificador | (Expresion) 
        private void Factor()
        {
            if(getClasificacion()==Tipos.numero)
            {
               //Console.Write(getContenido() + " ");
               if(ValorToEnum(float.Parse(getContenido())) > Dominante)
               {
                   Dominante = ValorToEnum(float.Parse(getContenido()));
                   //hacer lo mismo con las variables 
               }
               SE.Push(float.Parse(getContenido())); //ingresamos al stack
               asm.WriteLine("MOV AX, " + getContenido());
               asm.WriteLine("PUSH AX");
               Match(Tipos.numero);
            }
            else if(getClasificacion()==Tipos.identificador)
            {
                string variable = getContenido();
                if(!Existe(variable)) //buscarlos en Match(Tipos.identificador) y agregar excepcion
                {
                    throw new Error("ERROR DE SINTAXIS: Variable no declarada: " + getContenido(),linea,log);
                }
                Match(Tipos.identificador);
                if(GetTipo(variable) > Dominante)//Modifica el dominante
                {
                    Dominante = GetTipo(variable);
                }
                SE.Push(GetValor(variable));//metemos al stack la variable 
                asm.WriteLine("MOV AX, " + variable);
                asm.WriteLine("PUSH AX");
            }
            else
            {
               bool HuboCast=false;
               Variable.TDatos TipoCast= Variable.TDatos.sinTipo;
               Match("(");
               if(getClasificacion()==Tipos.tipoDato)
               {
                   HuboCast = true;
                   TipoCast = StrignToEnum(getContenido());// guardamos el contenido para HuboCast
                   Match(Tipos.tipoDato);//Modificar el dominante sin condición
                   Match(")");
                   Match("(");
               }
               Expresion();
               Match(")");
               if(HuboCast)
               {
                   float N1 = SE.Pop();
                   asm.WriteLine("POP");
                   Dominante = TipoCast;
                   SE.Push(ValorCasteado(N1,TipoCast)); //desarrollar el metodo para castear char y int
                   asm.WriteLine("PUSH");
               }
            }
        }
    }
}