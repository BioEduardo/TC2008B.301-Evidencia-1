#----------------------------------------------------------
# M1. Actividad
# Este programa representa a los agentes Robot y Basura
# 
# Date: 11-Nov-2022
# Authors:
#           Eduardo Joel Cortez Valente A01746664
#           Paulo Ogando Gulias A01751587
#----------------------------------------------------------

from mesa import Agent

# Representacion de un robot agente
class Robot(Agent):

    # Inicializa los valores
    def __init__(self, unique_id, model):
        super().__init__(unique_id, model)
        self.cleaned = 0
        self.movements = 0

    # Define el movimiento aleatorio de los agentes
    def move(self):
        possible_steps = self.model.grid.get_neighborhood(
            self.pos,
            moore=True,
            include_center=False)
        new_position = self.random.choice(possible_steps)
        self.model.grid.move_agent(self, new_position)

    # Limpia la basura al cambiar su estado
    def clean_trash(self, other):
        other.state = 1
        self.cleaned += 1
        self.model.num_trash -= 1
        self.movements -= 1

    # Representa un paso, donde se coteja si la celda esta limpia
    def step(self):
        cellmates = self.model.grid.get_cell_list_contents([self.pos])
        self.movements += 1
        if len(cellmates) > 0:
            for other in cellmates:
                if (isinstance(other, Basura)):
                    if (other.state == 0):
                        self.clean_trash(other)
                    else:
                        self.move()
                else:
                    self.move()
        else:
            self.move()


# Representa al agente basura
class Basura(Agent):

    UNCLEAN = 0
    CLEAN = 1

    # Inicializa sus valores de instacia
    def __init__(self, unique_id, model, init_state=UNCLEAN):
        super().__init__(unique_id, model)
        self.state = init_state

    # Funcion para cotejar si esta limpio. Retorna True si lo esta
    def isClean(self):
        if self.state == self.CLEAN:
            return True
        else:
            return False

    # Representa un paso
    def step(self):
        pass
