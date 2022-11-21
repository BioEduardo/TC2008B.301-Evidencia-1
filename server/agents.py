#----------------------------------------------------------
# Evidencia 1. Actividad Integradora
# Este programa representa al agente robot y los objetos
# pared, puerta, caja y estanteria
# 
# Date: 21-Nov-2022
# Authors:
#           Eduardo Joel Cortez Valente A01746664
#           Paulo Ogando Gulias A01751587
#           David Damián Galán A01752785
#           José Ángel García Gómez A01745865
#----------------------------------------------------------

from mesa import Agent

class Robot(Agent):
    """
    Agente robot
    Attributes:
        unique_id: Agent's ID 
    """

    def __init__(self, unique_id, model):
        """
        Crea a un agente robot. Tiene
            - Un ID
            - Un color
            - Si tiene un caja o no
            - La posición al estante al cual se intentará mover
        """
        super().__init__(unique_id, model)
        self.movements = 0
        self.color = "green"
        self.has_box = False
        self.estante_to_move = (0,0)
 
    def move(self):
        """ 
        Determina si el agente se puede mover a una zona. De manera aleatoria
        """
        possible_steps = self.model.grid.get_neighborhood(
            self.pos,
            moore=True,
            include_center=False)
        new_position = self.random.choice(possible_steps)
        cellmates = self.model.grid.get_cell_list_contents([new_position])
        for element in cellmates:
            if isinstance(element, Robot) or isinstance(element, Pared) or isinstance(element, Puerta):
                new_position = self.pos
        self.model.grid.move_agent(self, new_position)

    def calcularDistancia(self, posEstante, posMover):
        """
        Es el calculo de una distancia dada dos posiciones
        """
        nx, ny = posEstante
        ax, ay = posMover
        return (abs(ax - nx) + abs(ay - ny))

    def moveToEstante(self):
        """
        Reprsenta el movimiento del robot hacia el estante mas cercano a el
        A partir de la lista de posiciones donde se encuentran los estantes, calcula cual es el mas cercano a el
        Despues, calcula cual de sus 8 posibles movimientos le acercará a dicho estante
        Ese movimiento será el que seleccionará para realizar el movimiento en está iteración
        """
        possible_steps = self.model.grid.get_neighborhood(
            self.pos,
            moore=True,
            include_center=False)
        
        self.estante_to_move = (0,0)
        distanciaMinima = 10000
        for posEstante in self.model.pos_estantes:
            distanciaNueva = self.calcularDistancia(posEstante, self.pos)
            if distanciaNueva < distanciaMinima:
                distanciaMinima = distanciaNueva
                self.estante_to_move = posEstante

        place_to_move = (0,0)
        for step in possible_steps:
            distanciaNuevaParaMover = self.calcularDistancia(self.estante_to_move, step)
            if distanciaNuevaParaMover < distanciaMinima:
                distanciaMinima = distanciaNuevaParaMover
                place_to_move = step
            
        cellmates = self.model.grid.get_cell_list_contents([place_to_move])
        for element in cellmates:
            if isinstance(element, Robot) or isinstance(element, Pared) or isinstance(element, Puerta):
                place_to_move = self.pos

        self.model.grid.move_agent(self, place_to_move)
    
    def colocarCajaEstante(self, estante):
        """
        Reprsenta el colocar una caja en un estante
        """
        self.model.boxes_recolected += 1
        estante.current_boxes += 1
        self.color = "green"        
        self.has_box = False

    def recogerCaja(self, caja):
        """
        Reprsenta el recoger una caja
        """
        caja.color = "white"
        caja.status = True
        self.color = "orange"
        self.has_box = True

    def step(self):
        """
        Reprsenta un paso
            - Si el robot tiene una caja, se moverá a un estante
            - Si no lo tiene, se moverá aletoriamente
            - Observará los objetos que se encuentran en la celda en la que esta
                - Si es un Estante y es el estante al que deseaba moverse:
                    - Y dicho estante tiene menos de 5 cajas y el robot tiene una caja:
                        - Deposita la caja
                    - Si el estante ya tiene 5 cajas:
                        - Elimina ese estante de su lista y busca uno nuevo
                - Si es una caja y el no tiene ninguna caja:
                    - Recogera esa caja
        """
        if self.has_box:
            self.moveToEstante()
        else:
            self.move()
        self.movements += 1
        cellmates = self.model.grid.get_cell_list_contents([self.pos])
        for agent in cellmates:
            if type(agent) is Estante and self.estante_to_move == self.pos:
                if agent.current_boxes < 5 and self.has_box:
                    self.colocarCajaEstante(agent)
                elif agent.current_boxes == 5:
                    if self.estante_to_move in self.model.pos_estantes:
                        self.model.pos_estantes.remove(self.estante_to_move)
            elif type(agent) is Caja and not self.has_box and agent.color == "red":
                self.recogerCaja(agent)
                

class Caja(Agent):
    """
    Caja agent. Just to add Caja to the grid.
    """

    def __init__(self, unique_id, model, init_status=False):
        """
        Crea a un agente caja. Tiene:
            - Un ID
            - Un color
            - Un estado 'False' inicial. Esto indica que no lo
              lo han recogido aún
        """
        super().__init__(unique_id, model)
        self.color = "red"
        self.status = init_status

    def step(self):
        """
        Reprsenta un paso
        """
        pass


class Estante(Agent):
    """
    Estante agent. Just to add Estante to the grid.
    """
    
    def __init__(self, unique_id, model):
        """
        Crea a un agente Estante. Tiene
            - Un ID
            - La cantidad actual de cajas que contiene
            - La cantidad maxima de cajas que puede contener
        """
        super().__init__(unique_id, model)
        self.current_boxes = 0
        self.max_boxes = 5
    
    def step(self):
        """
        Reprsenta un paso
        """
        pass


class Pared(Agent):
    """
    Pared agent. Just to add Pared to the grid.
    """
    def __init__(self, unique_id, model):
        """
        Crea a un agente Pared. Tiene
            - Un ID
        """
        super().__init__(unique_id, model)

    def step(self):
        """
        Reprsenta un paso
        """
        pass


class Puerta(Agent):
    """
    Puerta agent. Just to add Puerta to the grid.
    """
    def __init__(self, unique_id, model):
        """
        Crea a un agente Puerta. Tiene
            - Un ID
        """
        super().__init__(unique_id, model)

    def step(self):
        """
        Reprsenta un paso
        """
        pass
