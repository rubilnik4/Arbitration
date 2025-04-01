module Arbitration.Application.Environments.ApplicationTypes

open System.Threading.Tasks
open Arbitration.Application.Interfaces

type Query<'input, 'output> =
    Env -> 'input -> Task<'output>

type Command<'state, 'input, 'output> =
    Env -> 'state -> 'input -> Task<'output * 'state>